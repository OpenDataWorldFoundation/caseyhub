// Step 3+ — Dynamic Questions
import QuestionForm from "@/src/components/Permits/QuestionForm";
import WizardScreenWrapper from "@/src/components/Permits/WizardScreenWrapper";
import { useEvaluatePermit } from "@/src/hooks/permitChecker/useEvaluatePermit";
import { usePermitCheckerStore } from "@/src/store/permitChckerStore";
import { QuestionDto } from "@/src/types/permitchecker/responses";
import { router } from "expo-router";
import React, { useState } from "react";
import { Text, View } from "react-native";

export default function QuestionsScreen() {
  const [currentQuestions, setCurrentQuestions] = useState<QuestionDto[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [batchIndex, setBatchIndex] = useState(0); // used as a re-render key for QuestionForm

  const sessionId = usePermitCheckerStore((s) => s.sessionId);
  const normalisedAddress = usePermitCheckerStore((s) => s.normalisedAddress);
  const latitude = usePermitCheckerStore((s) => s.latitude);
  const longitude = usePermitCheckerStore((s) => s.longitude);
  const zoneCode = usePermitCheckerStore((s) => s.zoneCode);
  const overlayCodes = usePermitCheckerStore((s) => s.overlayCodes);
  const buildingTypeSlug = usePermitCheckerStore((s) => s.buildingTypeSlug);
  const buildingTypeDisplayName = usePermitCheckerStore((s) => s.buildingTypeDisplayName);
  const accumulatedAnswers = usePermitCheckerStore((s) => s.answers);
  const clausesInScope = usePermitCheckerStore((s) => s.clausesInScope);

  const mergeAnswers = usePermitCheckerStore((s) => s.mergeAnswers);
  const setClausesInScope = usePermitCheckerStore((s) => s.setClausesInScope);
  const setVerdict = usePermitCheckerStore((s) => s.setVerdict);

  const { mutateAsync: evaluate, isPending: isEvaluating } = useEvaluatePermit();

  // On first mount, trigger an evaluate call to get the initial questions.
  // This handles the case where building-type screen already got NeedsMoreInfo
  // but we need to read that response here. We re-trigger because mutation results
  // are not persisted across navigation — they live in the mutation hook instance.
  React.useEffect(() => {
    triggerEvaluate({});
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const triggerEvaluate = async (newAnswers: Record<string, unknown>) => {
    setError(null);

    // Merge new answers into accumulated state
    mergeAnswers(newAnswers);
    const allAnswers = { ...accumulatedAnswers, ...newAnswers };

    try {
      const response = await evaluate({
        sessionId: sessionId!,
        normalisedAddress: normalisedAddress!,
        latitude: latitude!,
        longitude: longitude!,
        zoneCode: zoneCode!,
        overlayCodes,
        buildingTypeSlug: buildingTypeSlug!,
        answers: allAnswers,
      });

      setClausesInScope(response.clausesInScope);

      if (response.status === "Conclusive") {
        setVerdict({
          outcome: response.outcome!,
          outcomeSummary: response.outcomeSummary!,
          triggeredRules: response.triggeredRules ?? [],
          clausesInScope: response.clausesInScope,
          assessmentId: response.assessmentId,
        });
        // Replace so back button from result goes to building-type, not questions
        router.replace("/(apps)/permits/permit-checker/result");
      } else if (response.nextQuestions && response.nextQuestions.length > 0) {
        setCurrentQuestions(response.nextQuestions);
        setBatchIndex((prev) => prev + 1); // reset QuestionForm local state
      } else {
        setError("Unexpected response from the server. Please go back and try again.");
      }
    } catch {
      setError("Something went wrong. Please try again.");
    }
  };

  const handleFormSubmit = (batchAnswers: Record<string, unknown>) => {
    triggerEvaluate(batchAnswers);
  };

  return (
    <WizardScreenWrapper
      stepLabel={`Assessing — ${buildingTypeDisplayName ?? "Your Project"}`}
      title="A few more details"
      subtitle="Answer the questions below so we can determine your permit requirements."
      clausesInScope={clausesInScope}
    >
      {currentQuestions.length > 0 && (
        <QuestionForm
          key={batchIndex} // remount when question batch changes to reset local state
          questions={currentQuestions}
          onSubmit={handleFormSubmit}
          isLoading={isEvaluating}
        />
      )}

      {error ? (
        <Text className="text-red-500 text-sm mt-4">{error}</Text>
      ) : null}
    </WizardScreenWrapper>
  );
}