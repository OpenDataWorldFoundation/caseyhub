import { useAuth } from "@/src/context/authContext";
import { authFetch } from "@/src/utils/authFetch";
import { useMutation } from "@tanstack/react-query";

interface LoginCredentials {
    email: string;
    password: string;
}

export const useLogin = () => {
    const {login} = useAuth();

    return useMutation({
        mutationFn: async (credentials: LoginCredentials) => {
            const response = await authFetch("/auth/login", null, {method: "POST", body: JSON.stringify(credentials)});
            
            if (!response.ok) throw new Error ("Invalid email or password");

            const data = await response.json();
            await login (data.token);
            return data;
        }
    });
}