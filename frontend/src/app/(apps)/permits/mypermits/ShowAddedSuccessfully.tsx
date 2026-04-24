import { Check } from "lucide-react-native";
import { Text, View } from "react-native"

const ShowAddedSuccessfully = () => {
    return(
        <View>
            <Check color="green" />
            <Text className="text-green-800"> Your Permit was Succesfully Saved! </Text>
        </View>
    )
}

export default ShowAddedSuccessfully;