-- in the entire app we use new booking instead change that to new lead
currently manager only do the bookings i need the same bookings also done by the receptionists , and the current booking flow has 4 modules now i only need these 3
in first module  we take First name, last name, mobile, email, village, mandal, ref name, ref mobile, Guest Aadhaar, Guest Pan, Address/Any Details / remarks 
-- in second module we take Function Date, Function Name, Meal Type(v/e), Meal Plan (breakfast, lunch, dinner, hitea), no of packs, gaurenteed packs, special instruction (more than one meal can mention details here), function assign to(manager dropdown), function hall(dropdown)
-- in third moduule show all these details taken in 1st and second modules here and option to print or download as pdf 

-- for function name dropdown it must fetch from the db currently we have these (birthday, reception, half-saree function, engagement, get-to gether, doti function, marriage) and this table structure must like id , name, sort_order 

-- for function hall dropdown we need the table structure like function name table

-- admin has option to create/edit/activate/deactivate the function name and their sort order and function hall name and sort order

-- new page for admin to see these leads with lead date, function date, name, phone, status, managed by, last follow up date the page must contain these sort and filters (lead date | function date, from date - to date, by manager) by default on page open it shows new to old leads 

-- after all these changes check the remaing pages data showing accordingly to this changes  
after done all these changes adudit new and updated pages for bugs/errors and fix them  and also give me sql code to add the missing tables or missing cols/rows in the changes1.sql file