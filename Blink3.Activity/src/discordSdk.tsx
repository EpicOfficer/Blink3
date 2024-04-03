import {DiscordSDK} from '@discord/embedded-app-sdk';

let discordSdk = new DiscordSDK(import.meta.env.VITE_CLIENT_ID, {
    disableConsoleLogOverride: true
});

export {discordSdk}