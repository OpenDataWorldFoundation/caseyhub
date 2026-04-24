import { authFetch } from "@/src/utils/authFetch"
import { useMutation } from "@tanstack/react-query"

interface RegisterProps {
    name: string
    email: string,
    password: string
}

export const useRegister = () => {
    return useMutation({
            mutationFn: async (credentials: RegisterProps) => {
                const response = await authFetch("/auth/register", null, {method: "POST", body: JSON.stringify(credentials)})
                if(!response.ok){
                    throw new Error ("Registration Failed.")
                }
            }
        }
    )
}