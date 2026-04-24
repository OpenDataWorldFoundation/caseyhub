import CustomButtonComponent from "@/src/components/Shared/CustomButtonComponent";
import { View, TextInput, Pressable, Text } from "react-native";

const Login = () => {
    return (
        <View className="flex-1 justify-center items-center px-6">
                <TextInput
                    placeholder="Username"
                    className="w-full border border-black-700 rounded-lg p-4"
                />
                <TextInput
                    placeholder="Password"
                    secureTextEntry
                    className="w-full border border-black-700 rounded-lg p-4"
                />
                <CustomButtonComponent label="Login" onClick={()=>console.log('Login!')} /> 
            </View>
    );
};

export default Login;