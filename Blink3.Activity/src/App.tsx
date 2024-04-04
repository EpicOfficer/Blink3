// noinspection JSIgnoredPromiseFromCall
import {AuthContext} from "./contexts/AuthContext.tsx";
import {useContext} from "react";

export default function App() {
    const authContext = useContext(AuthContext);
    
    if (!authContext) {
        return <p>Loading...</p>;
    }

    if (!authContext.authenticated) {
        return <p>Logging in...</p>;
    }
    
    return (
        <>
            <p>Hello world</p>
            <p>Logged in as {authContext?.auth?.user.username}</p>
        </>
    )
}