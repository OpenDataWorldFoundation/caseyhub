import PermitOptionCard from "@/src/components/Permits/PermitsOptionsCard";
import { router } from "expo-router";
import { Check, Locate, User } from "lucide-react-native";
import { Text, View } from "react-native"

const PermitHomePage = () => {
    const handlePress = (routePath: any) => {
        router.push(routePath);
    }
    return (
        <View>
            <PermitOptionCard optionName="My Permits" optionIcon={<User/>} onPress={() => handlePress('(apps)/permits/mypermits')} />
            <PermitOptionCard optionName="Check Permit Status" optionIcon={<Check />} onPress={() => handlePress('/(apps)/permits/CheckPermitStatus')} />
            <PermitOptionCard optionName="Permits Near Me" optionIcon={<Locate />} onPress={() => handlePress('/(apps)/permits/PermitsNearMe')} />
        </View>
    )
}

export default PermitHomePage;