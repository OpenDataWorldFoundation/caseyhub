
import CustomButtonComponent from "@/src/components/Shared/CustomButtonComponent";
import { router, Stack } from "expo-router"
import { Pressable, Text } from "react-native";

const PermitsLayout = () => {
    return (
        <Stack screenOptions={{headerShown: true}}> 
            <Stack.Screen name="index" options={{ title: 'Permits', headerLeft: () => <CustomButtonComponent label="Close" onClick={()=>router.back()}/>}} />
            <Stack.Screen name="permit-checker/index" options={{ title: 'Permit Checker' }} />
            <Stack.Screen name="CheckPermitStatus" options={{ title: 'Check Status' }} />
            <Stack.Screen name="PermitsNearMe" options={{ title: 'Nearby Permits' }} />
            <Stack.Screen name="mypermits/index" options={{ title: 'My Permits' }} />
            <Stack.Screen name="[applicationId]" options={{ title: 'Permit Details' }} />
        </Stack>
    )
}

export default PermitsLayout;