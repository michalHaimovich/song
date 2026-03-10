
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
        if (data) {
            sessionStorage.setItem('token', data);
            window.location.href = 'index.html';
        }
    })
        .catch(error => {
            console.error('Error:', error);
            alert("An error occurred while trying to log in. Please try again later.");
        })
}
function handleGoogleLogin(response) {
    // response.credential מכיל את האסימון הסודי שגוגל יצרה למשתמש
    
    fetch('/Loggin/GoogleLogin', {
        method: 'POST',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        // אנחנו שולחים לשרת ה-C# שלנו את האסימון של גוגל
        body: JSON.stringify({ Credential: response.credential })
    })
    .then(response => {
        if (!response.ok) {
            alert("התחברות דרך גוגל נכשלה בשרת שלנו.");
            throw new Error('Google login failed in C#');
        }
        return response.json();
    })
    .then(token => {
        if (token) {
            // השרת שלנו אישר! שומרים את הטוקן של השרת ועוברים דף
            sessionStorage.setItem('token', token);
            window.location.href = 'index.html';
        }
    })
    .catch(error => {
        console.error('Error:', error);
    });
}