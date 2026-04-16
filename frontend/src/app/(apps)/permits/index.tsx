import PermitOptionCard from "@/src/components/Permits/PermitsOptionsCards";
import { router } from "expo-router";
import { User } from "lucide-react-native";
import { Text, View } from "react-native"

const PermitHomePage = () => {
    const handlePress = (routePath: any) => {
        router.push(routePath);
    }
    return (
        <View>
            <PermitOptionCard optionName="My Permits" optionIcon={<User/>} onPress={() => handlePress('(apps)/permits/mypermits')} />
        </View>
    )
}

export default PermitHomePage;