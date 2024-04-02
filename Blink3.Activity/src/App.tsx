// noinspection JSIgnoredPromiseFromCall

import {useEffect, useState} from 'react'
import reactLogo from './assets/react.svg'
import viteLogo from '/vite.svg'
import './App.css'
import {discordSdk} from './discordSdk';
import type {AsyncReturnType} from 'type-fest';

type Auth = AsyncReturnType<typeof discordSdk.commands.authenticate>;

function App() {
    const [count, setCount] = useState(0)
    const [auth, setAuth] = useState<Auth>();
    
    useEffect(() => {
        const setupDiscordSdk = async () => {
            await discordSdk.ready();

            // Authorize with Discord Client
            const {code} = await discordSdk.commands.authorize({
                client_id: import.meta.env.VITE_CLIENT_ID,
                response_type: 'code',
                state: '',
                prompt: 'none',
                scope: [
                    'identify',
                    'guilds',
                    'rpc.voice.read'
                ],
            });

            // Retrieve an access_token from your activity's server
            const response = await fetch('/api/auth/token', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    code,
                }),
            });
            const {access_token} = await response.json();

            // Authenticate with Discord client (using the access_token)
            setAuth(await discordSdk.commands.authenticate({
                access_token,
            }));

            if (auth == null) {
                throw new Error('Authenticate command failed');
            }
        }
        
        setupDiscordSdk();
    }, [auth, setAuth]);

    return (
        <>
            <div>
                <a href="https://vitejs.dev" target="_blank">
                    <img src={viteLogo} className="logo" alt="Vite logo"/>
                </a>
                <a href="https://react.dev" target="_blank">
                    <img src={reactLogo} className="logo react" alt="React logo"/>
                </a>
            </div>
            <h1>Vite + React</h1>
            <p>User: {auth?.user.username}</p>
            <div className="card">
                <button onClick={() => setCount((count) => count + 1)}>
                    count is {count}
                </button>
                <p>
                    Edit <code>src/App.tsx</code> and save to test HMR
                </p>
            </div>
            <p className="read-the-docs">
                Click on the Vite and React logos to learn more
            </p>
        </>
    )
}

export default App