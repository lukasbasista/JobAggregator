import React from "react";
import LatestJobList from "../components/LatestJobList";
import SearchBar from "../components/SearchBar";

const Home: React.FC = () => {
  return (
    <>
      <SearchBar />
      <LatestJobList />
    </>
  );
};

export default Home;
