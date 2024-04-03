import * as React from 'react';
import {discordSdk} from '../discordSdk';

import type {IGuildsMembersRead, TAuthenticateResponse, TAuthenticatedContext} from '../types';

const AuthenticatedContext = React.createContext<TAuthenticatedContext>({
    user: {
        id: '',
        username: '',
        discriminator: '',
        avatar: null,
        public_flags: 0,
    },
    access_token: '',
    scopes: [],
    expires: '',
    application: {
        rpc_origins: undefined,
        id: '',
        name: '',
        icon: null,
        description: '',
    },
    guildMember: null,
});

export function AuthenticatedContextProvider({children}: {children: React.ReactNode}) {
    const authenticatedContext = useAuthenticatedContextSetup();

    if (authenticatedContext == null) {
        return <div>loading...</div>
    }
    
    return <AuthenticatedContext.Provider value={authenticatedContext}>{children}</AuthenticatedContext.Provider>;
}

export function useAuthenticatedContext() {
    return React.useContext(AuthenticatedContext);
}

/**
 * This is a helper hook which is used to connect your embedded app with Discord and Colyseus
 */
function useAuthenticatedContextSetup() {
    const [auth, setAuth] = React.useState<TAuthenticatedContext | null>(null);
    const settingUp = React.useRef(false);

    React.useEffect(() => {
        const setUpDiscordSdk = async () => {
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
                    'guilds.members.read',
                    'rpc.voice.read',
                ],
            });

            // Retrieve an access_token from api
            const response = await fetch('/api/auth/token', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(code),
            });
            const {access_token} = await response.json();

            // Authenticate with Discord client (using the access_token)
            const newAuth: TAuthenticateResponse = await discordSdk.commands.authenticate({
                access_token,
            });

            // Get guild specific nickname and avatar, and fallback to user name and avatar
            const guildMember: IGuildsMembersRead | null = await fetch(
                `/discord/api/users/@me/guilds/${discordSdk.guildId}/member`,
                {
                    method: 'get',
                    headers: {Authorization: `Bearer ${access_token}`},
                }
            )
            .then((j) => j.json())
            .catch(() => {
                return null;
            });

            // Done with discord-specific setup

            setAuth({...newAuth, guildMember});
        };

        if (!settingUp.current) {
            settingUp.current = true;
            setUpDiscordSdk();
        }
    }, []);
    
    return auth;
}