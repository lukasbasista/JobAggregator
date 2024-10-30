import React, { useEffect, useState } from "react";
import { useLocation } from "react-router-dom";
import NavBar from "../components/NavBar";
import SearchBar from "../components/SearchBar";
import JobList from "../components/JobList";
import Footer from "../components/Footer";

const SearchResults: React.FC = () => {
  const location = useLocation();
  const [queryParams, setQueryParams] = useState<Record<string, string>>({});

  useEffect(() => {
    const params = new URLSearchParams(location.search);
    const queryObj = Object.fromEntries(params.entries());
    setQueryParams(queryObj);
  }, [location.search]);

  return (
    <>
      <NavBar />
      <SearchBar />
      <JobList
        fetchUrl="/JobPostings"
        title="Výsledky vyhledávání"
        queryParams={queryParams}
      />
      <Footer />
    </>
  );
};

export default SearchResults;
