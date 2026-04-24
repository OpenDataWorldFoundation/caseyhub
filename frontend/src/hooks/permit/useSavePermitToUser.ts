import { useAuth } from "@/src/context/authContext"
import { authFetch } from "@/src/utils/authFetch"
import { useMutation } from "@tanstack/react-query"

const saveUserPermit = async (userToken: string, applicationNumber: string) => {
    const response = await authFetch(`/Permits/savePermitToUser`, userToken, {method: "POST", body: JSON.stringify({applicationNumber})})
    if(!response.ok){
        throw new Error("Response NOT OK");
    }

    return response.json();
}

export const useSavePermitToUser = () => {
    const {userToken} = useAuth();
    return useMutation({
        mutationFn: async (applicationNumber: string) => {
            if(!userToken){
                throw new Error ("User Token not available");
            }
            return saveUserPermit(userToken, applicationNumber);
        }
    })
}