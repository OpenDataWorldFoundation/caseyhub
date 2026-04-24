
import { CustomNavbar } from "@/src/components/Navigation/CustomNavbar";
import { Tabs } from "expo-router"
import { CircleUserRound, Home, Menu } from "lucide-react-native";

const TabLayout = () => {
    return(
        <Tabs tabBar={(props) => <CustomNavbar {...props} /> } screenOptions={{headerShown: false}}> 
            <Tabs.Screen name="index" options={{
                title:'Home',
                tabBarIcon: ({color, size}) => <Home color={color} size={size} />
            }} />
            <Tabs.Screen name="Menu" options={{
                title:'Menu',
                tabBarIcon: ({color, size}) => <Menu color={color} size={size} />
            }} />
            <Tabs.Screen name="profile" options={{
                title:'Profile',
                tabBarIcon: ({color, size}) => <CircleUserRound color={color} size={size} />
            }} />
        </Tabs>
    )
}

export default TabLayout;
