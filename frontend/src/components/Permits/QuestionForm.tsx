import { QuestionDto } from "@/src/types/permitchecker/responses";
import React, { useState } from "react";
import { ActivityIndicator, Text, TouchableOpacity, View } from "react-native";
import DynamicQuestionField from "./DynamicQuestionField";

interface QuestionFormProps {
  questions: QuestionDto[];
  onSubmit: (answers: Record<string, unknown>) => void;
  isLoading: boolean;
}

const QuestionForm: React.FC<QuestionFormProps> = ({ questions, onSubmit, isLoading }) => {
  // Local draft: only tracks answers for THIS batch of questions
  const [draftAnswers, setDraftAnswers] = useState<Record<string, unknown>>({});

  const handleAnswer = (fieldKey: string, value: unknown) => {
    setDraftAnswers((prev) => ({ ...prev, [fieldKey]: value }));
  };

  const allAnswered = questions.every((q) => {
    const val = draftAnswers[q.fieldKey];
    if (val === undefined || val === null || val === "") return false;
    if (Array.isArray(val) && val.length === 0) return false;
    return true;
  });

  return (
    <View>
      {questions
        .slice()
        .sort((a, b) => a.displayOrder - b.displayOrder)
        .map((question) => (
          <DynamicQuestionField
            key={question.fieldKey}
            question={question}
            value={draftAnswers[question.fieldKey]}
            onAnswer={handleAnswer}
          />
        ))}

      {isLoading ? (
        <ActivityIndicator size="small" color="#000000" className="mt-4" />
      ) : (
        <TouchableOpacity
          className={`rounded-lg py-4 items-center mt-4 ${
            allAnswered ? "bg-black" : "bg-gray-300"
          }`}
          onPress={() => onSubmit(draftAnswers)}
          disabled={!allAnswered || isLoading}
          activeOpacity={0.8}
        >
          <Text
            className={`font-semibold text-base ${allAnswered ? "text-white" : "text-gray-500"}`}
          >
            Continue
          </Text>
        </TouchableOpacity>
      )}
    </View>
  );
};

export default QuestionForm;