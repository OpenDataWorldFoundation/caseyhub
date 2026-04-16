import { useLocalSearchParams } from "expo-router"
import { Text, View } from "react-native"

const PermitDetail = () => {
    const {permitId} = useLocalSearchParams();
    return (
        <View>
            <Text> Showing deets for Permit ID: {permitId} </Text>
        </View>
    )
}

export default PermitDetail;