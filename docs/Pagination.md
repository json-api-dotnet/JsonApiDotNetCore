---
currentMenu: pagination
---

# Pagination

Resources can be paginated. 
The following query would set the page size to 10 and get page 2.

```
?page[size]=10&page[number]=2
```

If you would like pagination implemented by default, you can specify the page size
when setting up the services:
