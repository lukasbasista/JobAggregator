import React from "react";
import { Grid, Card, CardActionArea, CardContent, Skeleton } from "@mui/material";

interface SkeletonLoaderProps {
  pageSize: number;
}

const SkeletonLoader: React.FC<SkeletonLoaderProps> = ({ pageSize }) => {
  return (
    <Grid container spacing={4}>
      {[...Array(pageSize)].map((_, index) => (
        <Grid item key={index} xs={12} sm={6} md={4}>
          <Card
            sx={{
              height: "100%",
              display: "flex",
              flexDirection: "column",
            }}
          >
            <CardActionArea sx={{ flexGrow: 1 }}>
              <Skeleton variant="rectangular" height={140} />
              <CardContent>
                <Skeleton variant="text" height={30} width="80%" />
                <Skeleton variant="text" width="60%" />
                <Skeleton variant="text" width="40%" />
                <Skeleton variant="text" width="50%" />
              </CardContent>
            </CardActionArea>
          </Card>
        </Grid>
      ))}
    </Grid>
  );
};

export default SkeletonLoader;
