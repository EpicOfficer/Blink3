import type {AsyncReturnType} from 'type-fest';
import {discordSdk} from "./discordSdk.tsx";
export type TAuthenticationResponse = AsyncReturnType<typeof discordSdk.commands.authenticate>;