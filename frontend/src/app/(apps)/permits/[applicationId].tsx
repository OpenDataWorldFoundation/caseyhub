import { useGetPermitByAppNumber } from "@/src/hooks/permit/useGetPermitByAppNumber";
import { useLocalSearchParams } from "expo-router"
import { ActivityIndicator, Text, View } from "react-native"

const PermitDetail = () => {
    const {applicationId} = useLocalSearchParams();
    const appId = applicationId as string;
    const {data: permit, isLoading} = useGetPermitByAppNumber(appId);

    if (isLoading) return <ActivityIndicator />
    return (
        <View>
            <Text> Application Number: {permit?.applicationNumber} </Text>
            <Text> Application Description: {permit?.description} </Text>
            <Text> Application Address: {permit?.address} </Text>
            <Text> Application Lodged Date: {permit?.lodgedDate} </Text>
            <Text> Application Current Stage: {permit?.stageDecision} </Text>
            {permit?.decisionDate &&  <Text> Application Decision Date: {permit?.decisionDate} </Text> }
        </View>
    )
}

export default PermitDetail;