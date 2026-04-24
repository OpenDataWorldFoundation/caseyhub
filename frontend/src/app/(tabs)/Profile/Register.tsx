import CustomButtonComponent from "@/src/components/Shared/CustomButtonComponent";
import { Pressable, TextInput, View } from "react-native"

const Register = () => {
    return (
        <View className="flex-1 justify-center items-center px-6">
                <TextInput
                    placeholder="Full Name"
                    className="w-full border border-black-700 rounded-lg p-4"
                />
                <TextInput
                    placeholder="Username"
                    className="w-full border border-black-700 rounded-lg p-4"
                />
                <TextInput
                    placeholder="Password"
                    secureTextEntry
                    className="w-full border border-black-700 rounded-lg p-4"
                />
                <CustomButtonComponent label="Register" onClick={()=>console.log('Register!')} /> 
            </View>
    )
}

export default Register;