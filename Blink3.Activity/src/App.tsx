// noinspection JSIgnoredPromiseFromCall
import {AuthenticatedContextProvider} from './hooks/useAuthenticatedContext';

function App() {
    return (
        <AuthenticatedContextProvider>
            <p>Hello</p>
        </AuthenticatedContextProvider>
    )
}

export default App