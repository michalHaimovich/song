const uri = '/Song';
let instruments = [];
const token = localStorage.getItem('token');
let currentUser = null;

// פונקציית עזר לפענוח הטוקן בבטחה
function parseJwt(jwtToken) {
    try {
        const base64Url = jwtToken.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));
        return JSON.parse(jsonPayload);
    } catch (e) {
        return null;
    }
}

function initPage() {
    if (!token) return;
    
    // שליפת נתונים מהטוקן
    currentUser = parseJwt(token);
    
    // מציאת השדה של התפקיד (Role) - השם המדויק תלוי באיך שהגדרת בשרת
    // לרוב מיוצג תחת הקישור הארוך או 'role'
    const roleClaim = currentUser['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || currentUser['userType'] || currentUser['role'];
    const isAdmin = roleClaim === 'admin';
    const username = currentUser['username'] || 'Unknown User';

    // עדכון כותרת
    document.getElementById('user-greeting').innerText = isAdmin 
        ? `Hello, ${username} (Admin)` 
        : `Hello, ${username}`;

    // הצגת שדות ה-UserID למנהל בלבד
    if (isAdmin) {
        document.getElementById('add-userId').style.display = 'inline-block';
        document.getElementById('edit-userId').style.display = 'inline-block';
    }

    // חיבור אירועי submit
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
    event.preventDefault(); // מניעת רענון הדף

    const roleClaim = currentUser['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || currentUser['userType'];
    const isAdmin = roleClaim === 'admin';
    
    // אם הוא מנהל, ניקח את מה שהקליד בשדה. אם לא, ניקח מהטוקן.
    const userIdValue = isAdmin 
        ? parseInt(document.getElementById('add-userId').value.trim(), 10)
        : parseInt(currentUser.userID, 10);

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
        getItems();
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
    
    // מילוי ה-userId אם מדובר במנהל
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
        : parseInt(currentUser.userID, 10);

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

        let td1 = tr.insertCell(0);
        td1.appendChild(document.createTextNode(item.id));

        let td2 = tr.insertCell(1);
        td2.appendChild(document.createTextNode(item.name));

        let td3 = tr.insertCell(2);
        td3.appendChild(document.createTextNode(item.composer));

        let td4 = tr.insertCell(3);
        td4.appendChild(document.createTextNode(item.userId));

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