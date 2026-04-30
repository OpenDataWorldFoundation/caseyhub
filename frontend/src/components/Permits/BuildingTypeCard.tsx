import { BuildingTypeDto } from "@/src/types/permitchecker/responses";
import React from "react";
import { Text, TouchableOpacity, View } from "react-native";

interface BuildingTypeCardProps {
  buildingType: BuildingTypeDto;
  onPress: (buildingType: BuildingTypeDto) => void;
  isLoading?: boolean;
}

const BuildingTypeCard: React.FC<BuildingTypeCardProps> = ({
  buildingType,
  onPress,
  isLoading = false,
}) => {
  return (
    <TouchableOpacity
      className="border border-gray-200 rounded-lg p-4 mb-3 bg-white active:bg-gray-50"
      onPress={() => onPress(buildingType)}
      disabled={isLoading}
      activeOpacity={0.7}
    >
      <Text className="text-base font-semibold text-black">{buildingType.displayName}</Text>
      {buildingType.description ? (
        <Text className="text-sm text-gray-500 mt-1">{buildingType.description}</Text>
      ) : null}
    </TouchableOpacity>
  );
};

export default BuildingTypeCard;