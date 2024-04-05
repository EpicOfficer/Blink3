// noinspection JSIgnoredPromiseFromCall
import {AuthContext} from "./contexts/AuthContext.tsx";
import {useContext} from "react";

export default function App() {
    const authContext = useContext(AuthContext);
    
    return (
        <p>Logged in as {authContext?.auth?.user.username}</p>
    )
}