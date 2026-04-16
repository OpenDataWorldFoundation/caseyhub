
import { CustomNavbar } from "@/src/components/Navigation/CustomNavbar";
import { Tabs } from "expo-router"
import { Home } from "lucide-react-native";

const TabLayout = () => {
    return(
        <Tabs tabBar={(props) => <CustomNavbar {...props} /> } screenOptions={{headerShown: false}}> 
            <Tabs.Screen name="index" options={{
                title:'Home',
                tabBarIcon: ({color, size}) => <Home color={color} size={size} />
            }} />
        </Tabs>
    )
}

export default TabLayout;