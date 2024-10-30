import React from "react";
import JobList from "./JobList";

const LatestJobList: React.FC = () => {
  return (
    <JobList fetchUrl="/JobPostings/Latest" title="Nové inzeráty" />
  );
};

export default LatestJobList;
