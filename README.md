# CommentedNews-Functions

This is the back-end part of a broader project. The purpose of the broader project is the gather all news articles posted to /r/Denmark throughout the day and rank them by number of comments they receive. The purpose of this is to make it easy to see which news might be relevant to read based on the user interaction that they are getting.

This project contains functions that are used to facilitate gathering and serving the information needed by the UI. Specifically the functions are as follows:
1. Scrape (Timer Trigger) - Makes an API call to the Reddit API and gets all new posts that have been made. Afterwards it finds all posts that contain a news article. All relevant information is then extracted from those posts and stored in a database. This function runs every 15 minutes.
2. CleanDB (Timer Trigger) - Deletes all articles in the database that are older than 7 days. This function runs once every day.
3. GetArticles (HTTP Trigger) - Returns all articles stored in the database. 

These functions are also automatically deployed to the Azure Cloud.

Demonstration of result in the UI:
![til](https://i.imgur.com/omVHDO3.jpg)
