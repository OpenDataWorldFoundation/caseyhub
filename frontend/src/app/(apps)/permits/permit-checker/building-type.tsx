// Step 2 — Building Type Selection
import BuildingTypeCard from "@/src/components/Permits/BuildingTypeCard";
import WizardScreenWrapper from "@/src/components/Permits/WizardScreenWrapper";
import { useEvaluatePermit } from "@/src/hooks/permitChecker/useEvaluatePermit";
import { useGetBuildingTypes } from "@/src/hooks/permitChecker/useGetBuildingTypes";
import { usePermitCheckerStore } from "@/src/store/permitChckerStore";
import { BuildingTypeDto } from "@/src/types/permitchecker/responses";
import { router } from "expo-router";
import React, { useState } from "react";
import { ActivityIndicator, Text, View } from "react-native";

export default function BuildingTypeScreen() {
  const { data: buildingTypes, isLoading: isLoadingTypes, isError } = useGetBuildingTypes();
  const [error, setError] = useState<string | null>(null);
  const [selectedSlug, setSelectedSlug] = useState<string | null>(null);

  const sessionId = usePermitCheckerStore((s) => s.sessionId);
  const normalisedAddress = usePermitCheckerStore((s) => s.normalisedAddress);
  const latitude = usePermitCheckerStore((s) => s.latitude);
  const longitude = usePermitCheckerStore((s) => s.longitude);
  const zoneCode = usePermitCheckerStore((s) => s.zoneCode);
  const overlayCodes = usePermitCheckerStore((s) => s.overlayCodes);
  const clausesInScope = usePermitCheckerStore((s) => s.clausesInScope);

  const setBuildingType = usePermitCheckerStore((s) => s.setBuildingType);
  const setClausesInScope = usePermitCheckerStore((s) => s.setClausesInScope);
  const setVerdict = usePermitCheckerStore((s) => s.setVerdict);

  const { mutateAsync: evaluate, isPending: isEvaluating } = useEvaluatePermit();

  const handleSelectBuildingType = async (buildingType: BuildingTypeDto) => {
    setError(null);
    setSelectedSlug(buildingType.slug);
    setBuildingType(buildingType.slug, buildingType.displayName);

    try {
      const response = await evaluate({
        sessionId: sessionId!,
        normalisedAddress: normalisedAddress!,
        latitude: latitude!,
        longitude: longitude!,
        zoneCode: zoneCode!,
        overlayCodes,
        buildingTypeSlug: buildingType.slug,
        answers: {},
      });

      // Always update the sidebar with the latest clauses in scope
      setClausesInScope(response.clausesInScope);

      if (response.status === "Conclusive") {
        setVerdict({
          outcome: response.outcome!,
          outcomeSummary: response.outcomeSummary!,
          triggeredRules: response.triggeredRules ?? [],
          clausesInScope: response.clausesInScope,
          assessmentId: response.assessmentId,
        });
        router.push("/(apps)/permits/permit-checker/result");
      } else {
        // NeedsMoreInfo — navigate to questions screen
        router.push("/(apps)/permits/permit-checker/questions");
      }
    } catch (err) {
      setError("Something went wrong evaluating your selection. Please try again.");
      setSelectedSlug(null);
    }
  };

  return (
    <WizardScreenWrapper
      stepLabel="Step 2 — Building Type"
      title="What are you trying to build?"
      subtitle="Select the type of structure or work you're planning."
      clausesInScope={clausesInScope}
    >
      {isLoadingTypes && (
        <ActivityIndicator size="small" color="#000000" />
      )}

      {isError && (
        <Text className="text-red-500 text-sm">
          Could not load building types. Please go back and try again.
        </Text>
      )}

      {buildingTypes?.map((bt) => (
        <BuildingTypeCard
          key={bt.slug}
          buildingType={bt}
          onPress={handleSelectBuildingType}
          isLoading={isEvaluating && selectedSlug === bt.slug}
        />
      ))}

      {isEvaluating && (
        <View className="flex-row items-center gap-x-2 mt-2">
          <ActivityIndicator size="small" color="#000000" />
          <Text className="text-sm text-gray-500">Checking planning rules…</Text>
        </View>
      )}

      {error ? <Text className="text-red-500 text-sm mt-2">{error}</Text> : null}
    </WizardScreenWrapper>
  );
}