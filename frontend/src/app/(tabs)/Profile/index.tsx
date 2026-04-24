import CustomButtonComponent from "@/src/components/Shared/CustomButtonComponent";
import { useAuth } from "@/src/context/authContext";
import { router } from "expo-router";
import { Pressable, Text, View } from "react-native";

const ProfileHomePage = () => {
    const {isLoading, userToken, logout} = useAuth();
    if (isLoading) {
        return (
            <View className="flex-1 items-center justify-center">
                <Text>Loading...</Text>
            </View>
        );
    }

    return (
        <View className="flex-1 items-center justify-center">
            {!userToken ? (
                <View>
                    <Text>You aren't logged in!</Text>
                    <CustomButtonComponent label="Login" onClick={() => router.navigate('/(tabs)/Profile/Login')} />
                    <CustomButtonComponent label="Register" onClick={() => router.navigate('/(tabs)/Profile/Register')} />
                </View>
            ) : (
                <View>
                    <Text>You are logged in</Text>
                    <CustomButtonComponent label="Logout" onClick={logout} />
                </View>
            )}
        </View>
    )
}

export default ProfileHomePage;