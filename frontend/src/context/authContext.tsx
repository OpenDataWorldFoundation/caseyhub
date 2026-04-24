import { Children, createContext, ReactNode, useContext, useEffect, useState } from "react";
import { deleteToken, getToken, saveToken } from "../utils/tokenStorage";

interface AuthContextProps {
    userToken : string | null;
    isLoading: boolean;
    login: (token:string) => Promise<void>;
    logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextProps | undefined>(undefined);

export const AuthProvider = ({children}: {children: ReactNode}) => {
    const [userToken, setUserToken] = useState<string | null> (null);
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        const loadToken = async () => {
            const token = await getToken();
            if(token) setUserToken(token);
            setIsLoading(false);
        };
        loadToken();
    }, [])

    const login = async (token: string) => {
        await saveToken(token);
        setUserToken(token);
    }

    const logout = async () => {
        await deleteToken();
        setUserToken(null);
    }

    return (
        <AuthContext.Provider value={{userToken, isLoading, login, logout}} >
            {children}
        </AuthContext.Provider>
    )
}

export const  useAuth = () => {
    const context = useContext(AuthContext);
    if(context == undefined){
        throw new Error ("UseAuth Must be used within an Auth Provider");
    }
    return context;
}