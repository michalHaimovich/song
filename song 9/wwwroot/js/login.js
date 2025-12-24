
const uri='/User/Login';

function chack(){

    const id = document.getElementById('ID').value.trim();
    const pw = document.getElementById('PW').value.trim();
    const name =document.getElementById('name').value.trim();
    fetch(uri, {
        method: 'POST',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({Id : id , name : name , Password : pw})
    }).then(response => response.json()
    .then(data => {
        localStorage.setItem('token', data); // Ensure data contains the token
        window.location.href = 'index.html';
    })
    .catch(error => {
        alert("you are not in");
    })
    )
}