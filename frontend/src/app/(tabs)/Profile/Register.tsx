import CustomButtonComponent from "@/src/components/Shared/CustomButtonComponent";
import { useLogin } from "@/src/hooks/auth/useLogin";
import { useRegister } from "@/src/hooks/auth/useRegister";
import { router } from "expo-router";
import { useState } from "react";
import { Pressable, Text, TextInput, View } from "react-native"

const Register = () => {
    const {mutate: executeRegister, isPending: isRegistering} = useRegister();
    const {mutate: executeLogin, isPending: isLoggingIn} = useLogin();
    const [name, setName] = useState("");
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState("");
    const clearForm = () => {
        setName("");
        setEmail("");
        setPassword("");
        setError("");
    }
    const handleRegister = () => {
        if (!email.trim() || !password.trim()){
            setError("Email or Password is required dog")
        }
        const registerCredentials = {name, email, password};
        const loginCredentials = {email, password};
        executeRegister(registerCredentials, {
            onSuccess: ()=>{
                executeLogin(loginCredentials, {onSuccess: () => {router.navigate('/(tabs)'); clearForm()}, onError: (err)=>setError(err.message)})
            }, 
            onError: (err) => setError(err.message) })
    }
    return (
        <View className="flex-1 justify-center items-center px-6">
            {error && <Text> {error} </Text>}
            <TextInput
                placeholder="Full Name"
                className="w-full border border-black-700 rounded-lg p-4"
                value={name}
                onChangeText={setName}
            />
            <TextInput
                placeholder="Username"
                className="w-full border border-black-700 rounded-lg p-4"
                value={email}
                onChangeText={setEmail}
            />
            <TextInput
                placeholder="Password"
                secureTextEntry
                className="w-full border border-black-700 rounded-lg p-4"
                value={password}
                onChangeText={setPassword}
            />
            <CustomButtonComponent label="Register" onClick={()=>handleRegister()} /> 
        </View>
    )
}

export default Register;