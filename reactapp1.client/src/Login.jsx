import { useEffect, useState } from 'react';
//import './App.css';

function Login() {
    const [usernameC, setUsername] = useState();
    const [passwordC, setPassword] = useState();

    useEffect(() => {
        sendLoginInfo();
    }, []);

    async function sendLoginInfo() {
        const data = {
            username: usernameC,
            password: passwordC
        };

        fetch('login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(data),
        })
            .then((response) => {
                if (!response.ok) {
                    throw new Error('Invalid username or password in frontend');
                }
                return response.json();
            })
            .then(responseData => {
                localStorage.setItem('token', responseData.token);
            })
            .catch(error => {
                console.log(error);
            });

    }


    function userChange(event) {
        setUsername(event.target.value);
    }
    function passChange(event) {
        setPassword(event.target.value);
    }

    return (
        <div>
            <h1 id="tabelLabel">Login</h1>
            <div>
                <div>
                    <label>Username:</label>
                    <input type="text" name="username" onChange={userChange } />
                </div>
                <div>
                    <label>Password:</label>
                    <input type="password" name="password" onChange={passChange } />
                </div>
                <button onClick={sendLoginInfo}>Login</button>
            </div>
        </div>
    );
}

export default Login;
