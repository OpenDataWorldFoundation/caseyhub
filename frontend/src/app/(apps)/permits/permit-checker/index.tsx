import AddressSearchInput from "@/src/components/Permits/AddressSearchInput";
import WizardScreenWrapper from "@/src/components/Permits/WizardScreenWrapper";
import { useAddressLookup } from "@/src/hooks/permitChecker/useAddressLookup";
import { usePermitCheckerStore } from "@/src/store/permitChckerStore";
import { router } from "expo-router";
import React, { useState } from "react";
import { Text, View } from "react-native";

const PermitCheckerAddressScreen = () => {
  const [address, setAddress] = useState("");
  const [error, setError] = useState<string | null>(null);

  const setAddressLookupResult = usePermitCheckerStore((s) => s.setAddressLookupResult);
  const clausesInScope = usePermitCheckerStore((s) => s.clausesInScope);
  const reset = usePermitCheckerStore((s) => s.reset);

  // Reset any previous wizard state when the user starts fresh
  React.useEffect(() => {
    reset(); //resetting zustand store
  }, []);

  const { refetch, isFetching } = useAddressLookup(address);

  const handleSubmit = async () => {
    setError(null);
    if (!address.trim()) return;
    const result = await refetch();
    if (result.isError) {
      setError("Could not look up that address. Please check it and try again.");
      return;
    }

    if (result.isSuccess && result.data) {
      setAddressLookupResult({
        sessionId: result.data.sessionId,
        normalisedAddress: result.data.normalisedAddress,
        latitude: result.data.latitude,
        longitude: result.data.longitude,
        zoneCode: result.data.zoneCode,
        zoneDescription: result.data.zoneDescription,
        overlayCodes: result.data.overlayCodes,
        relevantClauses: result.data.relevantClauses,
      });
      router.push("/(apps)/permits/permit-checker/building-type");
    }
    
  };

  return (
    <WizardScreenWrapper
      stepLabel="Step 1 — Address"
      title="What's your property address?"
      subtitle="We'll look up your planning zone and overlays to guide the assessment."
      clausesInScope={clausesInScope}
      showBack={true}
    >
      <AddressSearchInput
        value={address}
        onChangeText={setAddress}
        onSubmit={handleSubmit}
        isLoading={isFetching}
        placeholder="E.g. 29 Scone Street, Cranbourne"
        submitLabel="Look Up Address"
        error={error}
      />
    </WizardScreenWrapper>
  );
}

export default PermitCheckerAddressScreen;