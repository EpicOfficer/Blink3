import * as React from 'react';
import {discordSdk} from '../discordSdk';

import type {TAuthenticationResponse} from '../types';
import {createContext, ReactNode, useEffect, useState} from "react";

interface ContextType {
    auth?: TAuthenticationResponse | null;
    authenticated: boolean;
}


interface AuthProviderProps {
    children: ReactNode; // Here we defined the type for children
}

export const AuthContext = createContext<ContextType | null>(null);

export const AuthProvider: React.FC<AuthProviderProps> = ({children}) => {
    const [auth, setAuth] = useState<TAuthenticationResponse | null>(null);

    useEffect(() => {
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
            setAuth(await discordSdk.commands.authenticate({
                access_token,
            }));
        };

        setUpDiscordSdk();
    }, [auth, setAuth()]);

    return (
        <AuthContext.Provider value={{ auth, authenticated: !!auth }}>
            {children}
        </AuthContext.Provider>
    );
}