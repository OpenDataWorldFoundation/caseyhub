import CustomButtonComponent from "@/src/components/Shared/CustomButtonComponent";
import { useCheckPermitExists } from "@/src/hooks/permit/useCheckPermitExists";
import { useSavePermitToUser } from "@/src/hooks/permit/useSavePermitToUser";
import { QueryClient, useQueryClient } from "@tanstack/react-query";
import { router } from "expo-router";
import { useState } from "react";
import { ActivityIndicator, Text, TextInput, View } from "react-native"

const AddPermit = () => {
    const [appNumber, setAppNumber] = useState("");
    const [error, setError] = useState("");
    const {mutate: savePermit, isPending: saveLoading} = useSavePermitToUser()
    const {refetch, isFetching } = useCheckPermitExists(appNumber);
    const queryClient = useQueryClient();
    const handleAdd = async () => {
        setError("");
        if (!appNumber.trim()) {
            setError("Application Number is required!");
            return;
        }
        const { data, error } = await refetch();
        if (error || !data) {
            setError("Application Number not found!");
            return;
        }

        savePermit(appNumber, {onSuccess: ()=>{
                queryClient.invalidateQueries({queryKey: ["user", "savedpermits"]})
                if(router.canGoBack()){
                    router.back();
                }else{
                    router.replace('/(apps)/permits/mypermits')
                }
            }, 
            onError: (err)=>setError(err.message)
        });

    };
    return (
        <View>
            <TextInput 
            placeholder="Enter your permit number here"
            value={appNumber}
            onChangeText={setAppNumber}> 
            </TextInput>
            {saveLoading || isFetching ? (<ActivityIndicator />):(<CustomButtonComponent label="Check & Add Permit" onClick={()=>handleAdd()}/>)}
            <Text className="text-red-500"> {error} </Text>
        </View>
    )
}

export default AddPermit;