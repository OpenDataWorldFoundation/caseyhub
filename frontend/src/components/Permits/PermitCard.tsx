import { Permit } from "@/src/types/types";
import { router } from "expo-router";
import { navigate } from "expo-router/build/global-state/routing";
import { Pressable, Text, View } from "react-native";

interface PermitCardProps{
    permitIcon: React.ReactNode;
    permit: Permit;

}

export const PermitCard = ({permitIcon, permit}: PermitCardProps) => {
    return (
        <View>
            <Pressable onPress={() => router.navigate(`/(apps)/permits/${permit.applicationNumber}`)}>
                {permitIcon} 
                <Text> Application Number: {permit.applicationNumber} </Text>
                <Text> Description: {permit.description} </Text>
                <Text> Address: {permit.address} </Text>
                <Text> Status: {permit.decisionStage} </Text>
            </Pressable>
        </View>
    )
}

