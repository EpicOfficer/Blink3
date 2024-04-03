// noinspection JSIgnoredPromiseFromCall

import {useEffect} from 'react'

import {DiscordSDK} from '@discord/embedded-app-sdk';
const discordSdk = new DiscordSDK(import.meta.env.VITE_CLIENT_ID, {disableConsoleLogOverride: true });

function App() {
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
                    'Accept': 'application/json'
                },
                body: JSON.stringify(code),
            });
            const {access_token} = await response.json();

            // Authenticate with Discord client (using the access_token)
            let auth = await discordSdk.commands.authenticate({
                access_token,
            });
            
            console.info("Authenticated", auth.user.username);
        }
        
        setupDiscordSdk();
    }, []);

    return (
        <div></div>
    )
}

export default App