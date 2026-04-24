import React from "react";
import { View, FlatList } from "react-native";
import { useRouter } from "expo-router";
import { Book } from "lucide-react-native";
import HomeHeader from "@/src/components/HomePage/HomeHeader";
import AnAppComponent from "@/src/components/HomePage/AnAppComponent";

const HomePage = () => {
  const router = useRouter();
  const APPS = [{
    id: "1",
    appName: "Permit Manager",
    appDescription: "Central place to view, save and manage your permits",
    routePath: "/permits",
    buttonLabel: "View",
    icon: <Book size={60} strokeWidth={1.5} color="black" />
  }]

  const handleClick = (routePath : any) => {
    router.push(routePath);
  }

  return (
    <FlatList data={APPS} keyExtractor={(item)=> item.id} numColumns={2} className="flex-1 bg-white safe-area-pt tab-bar-pb"
      columnWrapperStyle={{justifyContent: "space-between", marginBottom: 16}}
      ListHeaderComponent={<HomeHeader />}
      ListHeaderComponentStyle={{marginBottom: 20}}

      renderItem={({item})=>(
        <View className="w-[48%]">
          <AnAppComponent
            appIcon = {item.icon}
            appName={item.appName}
            appDescription={item.appDescription}
            buttonLabel={item.buttonLabel}
            buttonOnClick={()=>handleClick(item.routePath)}
          />
        </View>
      )}

      showsVerticalScrollIndicator={false}
    />
  )

}

export default HomePage;
