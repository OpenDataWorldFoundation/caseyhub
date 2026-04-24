import { PermitCard } from "@/src/components/Permits/PermitCard";
import CustomButtonComponent from "@/src/components/Shared/CustomButtonComponent";
import { useGetUserPermits } from "@/src/hooks/permit/useGetUserPermits";
import { router } from "expo-router";
import { Building2 } from "lucide-react-native";
import { ActivityIndicator, Pressable, Text, View } from "react-native"

const MyPermitsHomePage = () => {
    const {data: userPermits, isLoading} = useGetUserPermits();
    if(isLoading){
        return (<ActivityIndicator/>)
    }
    return (
        <View> 
            <Text className="text-3xl pb-5"> Welcome to My Permits page </Text>
            <Text> Your Permits: </Text>
            {userPermits?.length === 0 ? (
                <Text> You have no saved Permits</Text>
            ):(
                <View className="pb-10"> 
                    {userPermits?.map((permit)=> (<PermitCard permit={permit} permitIcon={<Building2/>} key={permit.applicationNumber}/>))}
                </View>
            )}
            <CustomButtonComponent label="Add Permit" onClick={()=>router.push('/(apps)/permits/mypermits/AddPermit') } />
        </View>
    )
}
export default MyPermitsHomePage;