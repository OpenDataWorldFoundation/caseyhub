import { View } from "react-native"
import { GreetComponent } from "./GreetComponent";
import WeatherComponent from "./WeatherComponent";
import { useAuth } from "@/src/providers/AuthProvider";


const HomeHeader = () => {
    const { user } = useAuth();
    const username = user?.name?.split(" ")[0] || "there";

    return (
        <View className="flex-row justify-between items-start mb-10 px-2">
            <GreetComponent username={username} />
            <WeatherComponent />
        </View>
    )
}

export default HomeHeader;
