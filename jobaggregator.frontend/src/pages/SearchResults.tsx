import React, { useEffect, useState } from "react";
import { useLocation } from "react-router-dom";
import SearchBar from "../components/SearchBar";
import JobList from "../components/JobList";

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
      <SearchBar />
      <JobList
        fetchUrl="/JobPostings"
        title="Výsledky vyhledávání"
        queryParams={queryParams}
      />
    </>
  );
};

export default SearchResults;
