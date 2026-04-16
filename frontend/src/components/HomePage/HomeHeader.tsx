import { View } from "react-native"
import { GreetComponent } from "./GreetComponent";
import WeatherComponent from "./WeatherComponent";


const HomeHeader = () => {

    return (
        <View className="flex-row justify-between items-start mb-10 px-2">
            <GreetComponent username="Chirag" />
            <WeatherComponent />
        </View>
    )
}

export default HomeHeader;