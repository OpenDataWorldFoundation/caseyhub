import { Permit } from "@/src/types";
import { useQuery } from "@tanstack/react-query";

const fetchNearbyPermits = async (userAddress: string, radius: number ): Promise<Permit[]> => {
    const url = new URL("http://localhost:8080/api/Permits/getNearbyPermits")
    url.searchParams.set("address", userAddress);
    url.searchParams.set("radiusKm", radius.toString());
    const response = await fetch(url.toString());
    if(!response.ok){
        throw new Error("Response came NOT OK");
    }

    return response.json();
}

export const useGetPermitsNearAddress = (userAddress: string, radius: number) => {
    return useQuery<Permit[], Error>({
        queryKey: ["permits", "nearby", userAddress, radius],
        queryFn: () => fetchNearbyPermits(userAddress, radius),
        enabled: !!userAddress
    })
}