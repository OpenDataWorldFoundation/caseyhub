import CustomButtonComponent from "@/src/components/Shared/CustomButtonComponent";
import { useCheckPermitExists } from "@/src/hooks/permit/useCheckPermitExists";
import { useGetPermitByAppNumber } from "@/src/hooks/permit/useGetPermitByAppNumber";
import { router } from "expo-router";
import { useState } from "react";
import { ActivityIndicator, Text, TextInput, View } from "react-native"


const CheckPermitStatus = () => {
    const [appNumber, setAppNumber] = useState("");
    const [searchError, setSearchError] = useState("");

    const {refetch, isFetching} = useCheckPermitExists(appNumber);
    const handleCheckStatus = async () => {
        setSearchError("");
        if (!appNumber.trim()) return;
        const result = await refetch();

        if (result.isSuccess && result.data){
            router.navigate(`/permits/${appNumber}`)
        }else if(result.isError){
            setSearchError("No Record Found for this application number.")
        }else if (result.isSuccess && !result){
            setSearchError("No Record Found.");
        }
    }
    return (
<View className="p-4">
            <Text className="mb-2 font-bold">Application Number:</Text>
            <TextInput 
                placeholder="E.g. VS26-0078" 
                className="bg-black text-white p-3 rounded mb-4"
                value={appNumber}
                onChangeText={setAppNumber}
                autoCapitalize="none"
            /> 
            {isFetching ? (
                <ActivityIndicator size="large" color="#0000ff" />
            ) : (
                <CustomButtonComponent 
                    label="Check Status" 
                    onClick={handleCheckStatus} 
                />
            )}
            
            {searchError ? (
                <Text className="text-red-500 mb-4">{searchError}</Text>
            ) : null}
        </View>
    )
}

export default CheckPermitStatus;