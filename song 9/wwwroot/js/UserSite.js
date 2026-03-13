const uri = '/User';
let instruments = [];
const token = sessionStorage.getItem('token');
const payloadBase64 = token.split('.')[1];
const decodedToken = JSON.parse(atob(payloadBase64));
const userRole = decodedToken['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
const username = decodedToken['username'] || 'Unknown';

// הגדרות ראשוניות לפי תפקיד
if (userRole === 'admin') {
    document.getElementById('addForm').style.display = 'block';
    document.getElementById('add-role').style.display = 'inline-block';
    document.getElementById('role-header').style.display = 'table-cell';
    document.getElementById('delete-header').style.display = 'table-cell';
}

// הצגת ברכה למשתמש
window.addEventListener('load', function() {
    const greeting = document.getElementById('user-greeting');
    if (greeting) {
        greeting.innerText = userRole === 'admin' 
            ? `Hello, ${username} (Admin)` 
            : `Hello, ${username}`;
    }
});

function getItems() {
    fetch(uri, {
        method: 'GET',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        }
    })
    .then(response => response.json())
    .then(data => _displayItems(data))
    .catch(error => console.error('Unable to get items.', error));
}

function addItem() {
    const addNameTextbox = document.getElementById('add-name');
    const addPasswordTextbox = document.getElementById('add-password');
    const addRoleTextbox = document.getElementById('add-role');

    const item = {
        Id: 0,
        name: addNameTextbox.value.trim(),
        Password: addPasswordTextbox.value.trim(),
        // מנהל יכול לקבוע תפקיד, משתמש רגיל תמיד יהיה 'user'
        Role: userRole === 'admin' ? (addRoleTextbox.value.trim() || 'user') : 'user'
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
    .then(() => {
        getItems();
        addNameTextbox.value = '';
        addPasswordTextbox.value = '';
        addRoleTextbox.value = '';
    })
    .catch(error => console.error('Unable to add item.', error));
}

function deleteItem(id) {
    fetch(`${uri}/${id}`, {
        method: 'DELETE',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        },
    })
    .then(() => getItems())
    .catch(error => console.error('Unable to delete item.', error));
}

function displayEditForm(id) {
    const item = instruments.find(item => item.Id === id || item.id === id);

    document.getElementById('edit-id').value = item.Id || item.id;
    document.getElementById('edit-id-display').innerText = `Editing User ID: ${item.Id || item.id}`;
    document.getElementById('edit-name').value = item.name;
    document.getElementById('edit-password').value = item.Password || item.password;
    
    document.getElementById('editForm').style.display = 'block';

    // הצגת עריכת תפקיד רק למנהל
    if (userRole === 'admin') {
        const roleInput = document.getElementById('edit-role');
        roleInput.style.display = 'inline-block';
        roleInput.value = item.Role || item.role || 'user';
    }
}

function updateItem() {
    const itemId = document.getElementById('edit-id').value;

    const item = {
        Id: parseInt(itemId, 10),
        name: document.getElementById('edit-name').value.trim(),
        Password: document.getElementById('edit-password').value.trim(),
        Role: userRole === 'admin' ? document.getElementById('edit-role').value.trim() : 'user'
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
    .then(() => {
        getItems();
        closeInput();
    })
    .catch(error => console.error('Unable to update item.', error));

    return false;
}

function closeInput() {
    document.getElementById('editForm').style.display = 'none';
}

function _displayItems(data) {
    const tBody = document.getElementById('musics');
    tBody.innerHTML = '';
    const button = document.createElement('button');

    data.forEach(item => {
        let tr = tBody.insertRow();
        let currentId = item.Id || item.id;

        // עמודה 0: ID (לכולם)
        tr.insertCell(0).innerText = currentId;

        // עמודה 1: שם (לכולם)
        tr.insertCell(1).innerText = item.name;

        // עמודה 2: תפקיד (רק למנהל)
        if (userRole === 'admin') {
            tr.insertCell(2).innerText = item.Role || item.role || 'user';
        }

        // עמודה הבאה: סיסמה
        let passwordCell = tr.insertCell(userRole === 'admin' ? 3 : 2);
        passwordCell.innerText = item.Password || item.password;

        // עמודה הבאה: עריכה
        let editCell = tr.insertCell(userRole === 'admin' ? 4 : 3);
        let editButton = button.cloneNode(false);
        editButton.innerText = 'Edit';
        editButton.setAttribute('onclick', `displayEditForm(${currentId})`);
        editCell.appendChild(editButton);

        // עמודה הבאה: מחיקה (רק למנהל)
        if (userRole === 'admin') {
            let deleteCell = tr.insertCell(5);
            let deleteButton = button.cloneNode(false);
            deleteButton.innerText = 'Delete';
            deleteButton.setAttribute('onclick', `deleteItem(${currentId})`);
            deleteCell.appendChild(deleteButton);
        }
    });

    instruments = data;
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