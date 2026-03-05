const uri = '/Song';
let instruments = [];
const token = localStorage.getItem('token');

function getItems() {
    const headers = {
        'Accept': 'application/json',
        'Content-Type': 'application/json'
    };
    
    if (token) {
        headers['Authorization'] = `Bearer ${token}`;
    }
    
    fetch(uri, {
        method: 'GET',
        headers: headers
    })
        .then(response => response.json())
        .then(data => _displayItems(data))
        .catch(error => console.error('Unable to get items.', error));
}

function addItem() {
    const addNameTextbox = document.getElementById('add-name');
    const addComposerTextbox = document.getElementById('add-composer');
    const decoded = JSON.parse(atob(token.split('.')[1]));
    const userId = decoded.userID;
    
    const item = {
        Id: 0,
        userId: userId,
        name: addNameTextbox.value.trim(),
        composer: addComposerTextbox.value.trim()
    };

    fetch(uri, {
            method: 'POST',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'Authorization' : `Bearer ${token}`
            },
            body: JSON.stringify(item)
        })
        .then(() => {
            getItems();
            addNameTextbox.value = '';
            addComposerTextbox.value = '';
        })
        .catch(error => console.error('Unable to add item.', error));
}

function deleteItem(id) {
    fetch(`${uri}/${id}`, {
            method: 'DELETE',
            headers:{
                'Authorization' : `Bearer ${token}`
            }
        })
        .then(() => getItems())
        .catch(error => console.error('Unable to delete item.', error));
}

function displayEditForm(id) {
    const item = instruments.find(item => item.id === id);
    document.getElementById('edit-id').value = item.id;
    document.getElementById('edit-name').value = item.name;
    document.getElementById('edit-composer').value = item.composer;
    document.getElementById('editForm').style.display = 'block';
}

function updateItem() {
    const itemId = document.getElementById('edit-id').value;
    const decoded = JSON.parse(atob(token.split('.')[1]));
    const userId = decoded.userID;
    
    const item = {
        id: parseInt(itemId, 10),
        userId: userId,
        name: document.getElementById('edit-name').value.trim(),
        composer: document.getElementById('edit-composer').value.trim()
    };

    fetch(`${uri}/${itemId}`, {
            method: 'PUT',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'Authorization' : `Bearer ${token}`
            },
            body: JSON.stringify(item)
        })
        .then(() => getItems())
        .catch(error => console.error('Unable to update item.', error));

    closeInput();

    return false;
}

function closeInput() {
    document.getElementById('editForm').style.display = 'none';
}


function _displayItems(data) {
    const tBody = document.getElementById('musics');
    tBody.innerHTML = '';

    document.getElementById('counter').innerText = `Total Songs: ${data.length}`;

    const button = document.createElement('button');

    data.forEach(item => {
        let editButton = button.cloneNode(false);
        editButton.innerText = 'Edit';
        editButton.setAttribute('onclick', `displayEditForm(${item.id})`);

        let deleteButton = button.cloneNode(false);
        deleteButton.innerText = 'Delete';
        deleteButton.setAttribute('onclick', `deleteItem(${item.id})`);

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
        td5.appendChild(editButton);
        td5.appendChild(document.createTextNode(' '));
        td5.appendChild(deleteButton);
    });

    instruments = data;
}