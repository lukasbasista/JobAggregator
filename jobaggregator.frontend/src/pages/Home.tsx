import React from 'react';
import NavBar from '../components/NavBar';
import Footer from '../components/Footer';
import LatestJobList from '../components/LatestJobList';
import SearchBar from '../components/SearchBar';

const Home: React.FC = () => {
  return (
    <>
      <NavBar />
      <SearchBar />
      <LatestJobList />
      <Footer />
    </>
  );
};

export default Home;
