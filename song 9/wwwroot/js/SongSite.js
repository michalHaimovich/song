const uri = '/Song';
let instruments = [];
const token = sessionStorage.getItem('token');
let currentUser = null;
let currentUserId = null;

// 1. קודם כל: פונקציית העזר לפענוח הטוקן
function parseJwt(jwtToken) {
    if (!jwtToken) return null;
    try {
        const base64Url = jwtToken.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(atob(base64).split('').map(function (c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));
        return JSON.parse(jsonPayload);
    } catch (e) {
        return null;
    }
}

// 2. פענוח הטוקן וחילוץ הנתונים (מבוצע מיד כשהדף עולה)
if (token) {
    currentUser = parseJwt(token);
    // חילוץ ה-ID לפי התקן החדש של מיקרוסופט
    currentUserId = currentUser['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
}

// 3. הגדרת SignalR (רק אם יש טוקן תקין)
if (token && currentUserId) {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/activityHub", {
            accessTokenFactory: () => token
        })
        .withAutomaticReconnect()
        .build();

    // --- ניהול חלון ההודעות (שומר 10 אחרונות) ---
    let notifications = [];

    function addNotification(message) {
        // נוסיף שעה מדויקת להודעה כדי שזה ייראה מקצועי
        const timeString = new Date().toLocaleTimeString();
        const fullMessage = `[${timeString}] ${message}`;

        // מוסיפים לתחילת המערך (כדי שההודעה החדשה תהיה למעלה)
        notifications.unshift(fullMessage);

        // אם יש יותר מ-10, מוחקים את הישנה ביותר (מהסוף)
        if (notifications.length > 10) {
            notifications.pop();
        }

        // מציירים מחדש את רשימת ההודעות ב-HTML
        const list = document.getElementById('notifications-list');
        if (list) {
            list.innerHTML = ''; // מנקים את הרשימה
            notifications.forEach(msg => {
                let li = document.createElement('li');
                li.style.padding = "5px 0";
                li.style.borderBottom = "1px solid #eee";
                li.innerText = msg;
                list.appendChild(li);
            });
        }
    }

    // --- האזנה להודעות מהשרת ---

    connection.on("ReceivePersonalActivity", (message, song) => {
        // קוראים לפונקציה החדשה שלנו במקום alert
        addNotification("🔔 " + message);
        getItems();
    });

    connection.on("ReceiveGlobalActivity", (message, song, performerId) => {
        if (performerId.toString() !== currentUserId.toString()) {
            // קוראים לפונקציה החדשה במקום alert
            addNotification("👑 " + message);
            getItems();
        }
    });

    connection.start()
        .then(() => console.log("✅ Connected to SignalR Hub successfully!"))
        .catch(err => console.error("❌ SignalR Connection Error: ", err));
}

// 4. אתחול הדף (הפונקציה שהייתה לך, עכשיו קצת יותר נקייה)
function initPage() {
    if (!currentUser) return;

    const roleClaim = currentUser['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || currentUser['userType'] || currentUser['role'];
    const isAdmin = roleClaim === 'admin';
    const username = currentUser['username'] || 'Unknown User';

    document.getElementById('user-greeting').innerText = isAdmin
        ? `Hello, ${username} (Admin)`
        : `Hello, ${username}`;

    // אם זה admin, הצג את הקישור לניהול יוזרים
    if (isAdmin) {
        const adminLink = document.getElementById('admin-link');
        if (adminLink) {
            adminLink.style.display = 'inline';
        }
    }

    if (isAdmin) {
        document.getElementById('add-userId').style.display = 'inline-block';
        document.getElementById('edit-userId').style.display = 'inline-block';
    }

    document.getElementById('add-form').addEventListener('submit', addItem);
    document.getElementById('edit-form').addEventListener('submit', updateItem);

    getItems();
}

function getItems() {
    fetch(uri, {
        method: 'GET',
        headers: {
            'Accept': 'application/json',
            'Authorization': `Bearer ${token}`
        }
    })
        .then(response => {
            if (!response.ok) throw new Error('Failed to fetch items');
            return response.json();
        })
        .then(data => _displayItems(data))
        .catch(error => console.error('Unable to get items.', error));
}

function addItem(event) {
    event.preventDefault();

    const roleClaim = currentUser['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || currentUser['userType'];
    const isAdmin = roleClaim === 'admin';

    const userIdValue = isAdmin
        ? parseInt(document.getElementById('add-userId').value.trim(), 10)
        : parseInt(currentUserId, 10); // שינוי חשוב: משתמשים ב-currentUserId שחילצנו בהתחלה

    const item = {
        id: 0,
        userId: userIdValue,
        name: document.getElementById('add-name').value.trim(),
        composer: document.getElementById('add-composer').value.trim()
    };

    fetch(uri, {
        method: 'POST',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(item)
    })
        .then(response => {
            if (!response.ok) throw new Error('Failed to add item');
            getItems();
            document.getElementById('add-name').value = '';
            document.getElementById('add-composer').value = '';
            if (isAdmin) document.getElementById('add-userId').value = '';
        })
        .catch(error => console.error('Unable to add item.', error));
}

function deleteItem(id) {
    fetch(`${uri}/${id}`, {
        method: 'DELETE',
        headers: {
            'Authorization': `Bearer ${token}`
        }
    })
        .then(response => {
            if (!response.ok) throw new Error('Failed to delete item');
            getItems();
        })
        .catch(error => console.error('Unable to delete item.', error));
}

function displayEditForm(id) {
    const item = instruments.find(i => i.id === id);
    if (!item) return;

    document.getElementById('edit-id').value = item.id;
    document.getElementById('edit-name').value = item.name;
    document.getElementById('edit-composer').value = item.composer;

    const roleClaim = currentUser['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || currentUser['userType'];
    if (roleClaim === 'admin') {
        document.getElementById('edit-userId').value = item.userId;
    }

    document.getElementById('editForm').style.display = 'block';
}

function updateItem(event) {
    event.preventDefault();

    const itemId = parseInt(document.getElementById('edit-id').value, 10);
    const roleClaim = currentUser['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || currentUser['userType'];
    const isAdmin = roleClaim === 'admin';

    const userIdValue = isAdmin
        ? parseInt(document.getElementById('edit-userId').value.trim(), 10)
        : parseInt(currentUserId, 10); // שינוי חשוב: משתמשים ב-currentUserId

    const item = {
        id: itemId,
        userId: userIdValue,
        name: document.getElementById('edit-name').value.trim(),
        composer: document.getElementById('edit-composer').value.trim()
    };

    fetch(`${uri}/${itemId}`, {
        method: 'PUT',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(item)
    })
        .then(response => {
            if (!response.ok) throw new Error('Failed to update item');
            getItems();
            closeInput();
        })
        .catch(error => console.error('Unable to update item.', error));
}

function closeInput() {
    document.getElementById('editForm').style.display = 'none';
}

function _displayItems(data) {
    const tBody = document.getElementById('musics');
    tBody.innerHTML = '';
    document.getElementById('counter').innerText = `Total Songs: ${data.length}`;

    data.forEach(item => {
        let tr = tBody.insertRow();
        tr.insertCell(0).appendChild(document.createTextNode(item.id));
        tr.insertCell(1).appendChild(document.createTextNode(item.name));
        tr.insertCell(2).appendChild(document.createTextNode(item.composer));
        tr.insertCell(3).appendChild(document.createTextNode(item.userId));

        let td5 = tr.insertCell(4);

        let editButton = document.createElement('button');
        editButton.innerText = 'Edit';
        editButton.onclick = () => displayEditForm(item.id);
        td5.appendChild(editButton);

        td5.appendChild(document.createTextNode(' '));

        let deleteButton = document.createElement('button');
        deleteButton.innerText = 'Delete';
        deleteButton.onclick = () => deleteItem(item.id);
        td5.appendChild(deleteButton);
    });

    instruments = data;
}
// פונקציה להצגה והסתרה של חלון ההודעות
function toggleNotifications() {
    const panel = document.getElementById('notifications-panel');
    
    // בודקים מה המצב הנוכחי של החלון והופכים אותו
    if (panel.style.display === 'none') {
        panel.style.display = 'block';
    } else {
        panel.style.display = 'none';
    }
}

// פונקציות להצגת פרטים אישיים
function showMyProfile() {
    if (!token) {
        alert('No token found');
        return;
    }

    fetch('/User/me', {
        method: 'GET',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        }
    })
    .then(response => {
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        return response.json();
    })
    .then(user => {
        document.getElementById('profile-id').innerText = user.id || user.Id || 'N/A';
        document.getElementById('profile-name').innerText = user.name || 'N/A';
        document.getElementById('profile-role').innerText = user.role || user.Role || 'N/A';
        
        const modal = document.getElementById('profileModal');
        if (modal) {
            modal.style.display = 'flex';
        }
    })
    .catch(error => {
        console.error('Unable to fetch user profile:', error);
        alert('Failed to load profile details');
    });
}

function closeProfileModal() {
    const modal = document.getElementById('profileModal');
    if (modal) {
        modal.style.display = 'none';
    }
}

function editProfile() {
    const name = document.getElementById('profile-name').innerText;
    document.getElementById('edit-profile-name').value = name;
    document.getElementById('profile-content').style.display = 'none';
    document.getElementById('profile-edit').style.display = 'block';
}

function cancelEditProfile() {
    document.getElementById('profile-content').style.display = 'block';
    document.getElementById('profile-edit').style.display = 'none';
}

function saveProfile() {
    const newName = document.getElementById('edit-profile-name').value.trim();
    const newPassword = document.getElementById('edit-profile-password').value;
    const id = document.getElementById('profile-id').innerText;

    if (!newName) {
        alert('Name is required');
        return;
    }

    const updateData = { name: newName, Id: parseInt(id) };
    if (newPassword) {
        updateData.Password = newPassword;
    }

    fetch(`/User/${id}`, {
        method: 'PUT',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(updateData)
    })
    .then(response => {
        if (!response.ok) {
            throw new Error('Update failed');
        }
    })
    .then(() => {
        document.getElementById('profile-name').innerText = newName;
        cancelEditProfile();
        alert('Profile updated successfully');
    })
    .catch(error => {
        console.error(error);
        alert('Error updating profile');
    });
}

// קריאה לפונקציה שמתחילה את הכל כשהקובץ נטען
initPage();