import { PermitCard } from "@/src/components/Permits/PermitCard";
import CustomButtonComponent from "@/src/components/Shared/CustomButtonComponent";
import { useGetPermitsNearAddress } from "@/src/hooks/permit/useGetPermitsNearAddress";
import { Permit } from "@/src/types";
import { router } from "expo-router";
import { navigate } from "expo-router/build/global-state/routing";
import { Building } from "lucide-react-native";
import { useState } from "react"
import { ActivityIndicator, FlatList, Text, TextInput, View } from "react-native"

const PermitsNearMe = () => {
    const [address, setAddress] = useState("");
    const [radius, setRadius] = useState(1);
    const {refetch, isFetching} = useGetPermitsNearAddress(address, radius);
    const [searchError, setSearchError] = useState("");
    const [permits, setPermits] = useState<Permit[]>();
    const  handleSearch = async () => {
        setSearchError("");
        if(!address.trim()) return null;

        const result = await refetch();
        if(result.isSuccess && result.data && result.data.length >0){
            setPermits(result.data);
        }else{
            setSearchError(`No permits found near ${address}`);
            setPermits([]);
        }
        
    }
    return (
        <View style={{ flex: 1 }}>
            
            <View style={{ padding: 16 }}>
                <Text className="text-4xl">Find Permits Near You</Text>

                <Text>Address:</Text>
                <TextInput
                    placeholder="E.g. 29 Scone Street"
                    value={address}
                    onChangeText={setAddress}
                    autoCapitalize="none"
                />

                {isFetching ? (
                    <ActivityIndicator size="large" color="#0000ff" />
                ) : (
                    <CustomButtonComponent label="Submit" onClick={handleSearch} />
                )}

                {searchError ? (
                    <Text className="text-red-500">{searchError}</Text>
                ) : null}
            </View>

            <FlatList
                data={permits}
                keyExtractor={(item) => item.applicationNumber}
                contentContainerStyle={{ padding: 16 }}
                ListEmptyComponent={
                    !address ? (
                        <Text>Enter an address to get started!</Text>
                    ) : null
                }
                renderItem={({ item }) => (
                    <PermitCard
                        permit={item}
                        permitIcon={<Building />}
                    />
                )}
            />
        </View>
    );
}

export default PermitsNearMe;