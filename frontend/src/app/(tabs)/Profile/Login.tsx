import CustomButtonComponent from "@/src/components/Shared/CustomButtonComponent";
import { useLogin } from "@/src/hooks/auth/useLogin";
import { router } from "expo-router";
import { useState } from "react";
import { View, TextInput, Text, ActivityIndicator } from "react-native";

const Login = () => {
    const {mutate: executeLogin, isPending: isLoggingIn} = useLogin();
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState("");
    const clearForm = () => {
        setEmail("");
        setPassword("");
        setError("");
    }
    const handleLogin = () => {
        setError("");
        if(!email.trim() || !password.trim()){
            setError("Username or Password cannot be blank");
            return;
        }
        const credentials = {email, password};
        executeLogin(credentials, {onSuccess: () => {router.navigate('/(tabs)'); clearForm()}, onError: (err)=>setError(err.message)})
    }
    return (
    
        <View className="flex-1 justify-center items-center px-6">
                {error && <Text> {error} </Text>}
                <TextInput
                    placeholder="Username"
                    className="w-full border border-black-700 rounded-lg p-4"
                    value={email}
                    onChangeText={setEmail}
                    keyboardType="email-address"
                />
                <TextInput
                    placeholder="Password"
                    secureTextEntry
                    className="w-full border border-black-700 rounded-lg p-4"
                    value={password}
                    onChangeText={setPassword}
                />
                {isLoggingIn ? (<ActivityIndicator/>) : (<CustomButtonComponent label="Login" onClick={()=>handleLogin()} />)}
            </View>
    );
};

export default Login;