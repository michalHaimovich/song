
const uri = '/Loggin/Login';

function chack() {

    const id = document.getElementById('ID').value.trim();
    const pw = document.getElementById('PW').value.trim();
    const name = document.getElementById('name').value.trim();
    fetch(uri, {
        method: 'POST',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ Id: id, name: name, Password: pw })
    }).then(response => {
        if (!response.ok) {
            alert("you are not a valid user pls try again");
            window.location.href = 'i.html'
        }
        else
            return response.json()
    }).then(data => {
        if(data){  
            localStorage.setItem('token', data);
            window.location.href = 'index.html';
        }
        })
        .catch(error => {

     })
}